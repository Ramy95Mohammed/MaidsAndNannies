using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace MaidsAndNannies.WebApi.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/admin/backup")]
[RequestSizeLimit(2L * 1024 * 1024 * 1024)]
public sealed class BackupController(IConfiguration configuration) : BaseApiController
{
    private readonly string _backupFolder = Path.Combine(Directory.GetCurrentDirectory(), "Backups");

    [HttpGet("create")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var csb = new SqlConnectionStringBuilder(configuration.GetConnectionString("DefaultConnection"));
        Directory.CreateDirectory(_backupFolder);

        var fileName = $"{csb.InitialCatalog}_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
        var filePath = Path.Combine(_backupFolder, fileName);

        await using var conn = new SqlConnection(csb.ConnectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = $"BACKUP DATABASE [{csb.InitialCatalog}] TO DISK = @path WITH FORMAT, INIT";
        cmd.CommandTimeout = 300;
        cmd.Parameters.AddWithValue("@path", filePath);
        await cmd.ExecuteNonQueryAsync(ct);

        var bytes = await System.IO.File.ReadAllBytesAsync(filePath, ct);
        return File(bytes, "application/octet-stream", fileName);
    }

    [HttpPost("restore")]
    public async Task<IActionResult> Restore([FromForm] IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0
            || !Path.GetExtension(file.FileName).Equals(".bak", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "الرجاء اختيار ملف نسخة احتياطية بصيغة .bak" });

        var connectionString = configuration.GetConnectionString("DefaultConnection")!;
        var csb = new SqlConnectionStringBuilder(connectionString);
        var dbName = csb.InitialCatalog;
        var masterConnectionString = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = "master" }.ConnectionString;

        Directory.CreateDirectory(_backupFolder);
        var filePath = Path.Combine(_backupFolder, $"restore_{DateTime.Now:yyyyMMdd_HHmmss}.bak");
        await using (var stream = System.IO.File.Create(filePath))
        {
            await file.CopyToAsync(stream, ct);
        }

        try
        {
            await using var masterConn = new SqlConnection(masterConnectionString);
            await masterConn.OpenAsync(ct);
            await ExecuteSqlAsync(masterConn,
                $"ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", ct);
            await ExecuteSqlAsync(masterConn,
                $"RESTORE DATABASE [{dbName}] FROM DISK = @path WITH REPLACE, RECOVERY", ct, filePath);
            await ExecuteSqlAsync(masterConn,
                $"ALTER DATABASE [{dbName}] SET MULTI_USER", ct);
        }
        catch
        {
            try
            {
                await using var fallback = new SqlConnection(masterConnectionString);
                await fallback.OpenAsync(ct);
                await ExecuteSqlAsync(fallback, $"ALTER DATABASE [{dbName}] SET MULTI_USER", ct);
            }
            catch { /* ignore */ }
            throw;
        }
        finally
        {
            System.IO.File.Delete(filePath);
        }

        return Ok(new { message = "تمت استعادة النسخة الاحتياطية بنجاح" });
    }

    private static async Task ExecuteSqlAsync(SqlConnection connection, string sql, CancellationToken ct, string? path = null)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandTimeout = 300;
        if (path is not null) cmd.Parameters.AddWithValue("@path", path);
        await cmd.ExecuteNonQueryAsync(ct);
    }
}