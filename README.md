# Maids and Nannies

منصة تربط صاحبات المنازل بعاملات المنزل من خلال ملفات موثقة، حجوزات، رسوم خدمة، وتقييمات متبادلة.

## الهيكل

- `src/MaidsAndNannies.Domain`: الكيانات وقواعد المجال.
- `src/MaidsAndNannies.Application`: العقود وحالات الاستخدام.
- `src/MaidsAndNannies.Infrastructure`: SQL Server وASP.NET Core Identity والتخزين الخاص للوثائق.
- `src/MaidsAndNannies.WebApi`: واجهة HTTP API.
- `src/maids-and-nannies-web`: Angular 20، العربية هي اللغة الافتراضية مع دعم الإنجليزية.

## التشغيل

```powershell
dotnet run --project src/MaidsAndNannies.WebApi
cd src/maids-and-nannies-web
npm start
```

تستخدم الواجهة المنفذ `4200` والـAPI المنفذ `5045` أثناء التطوير. يطبّق الـAPI migration تلقائياً في بيئة Development؛ ويمكن تطبيقها يدوياً عبر:

```powershell
dotnet ef database update --project src/MaidsAndNannies.Infrastructure --startup-project src/MaidsAndNannies.WebApi
```

عدّل `DefaultConnection` في `src/MaidsAndNannies.WebApi/appsettings.json` قبل إنشاء قاعدة البيانات. لا تستخدم قيمة `Jwt:Key` الموجودة في الملف خارج بيئة التطوير؛ عيّن سراً قوياً عبر إعدادات النشر أو User Secrets.

> وثائق الهوية وجواز السفر تحفظ في `private-uploads` ولا يجب تقديمها كملفات ثابتة أو إعادتها من نقاط عامة في الـAPI.

## المسار الحالي

1. تسجل صاحبة المنزل أو العاملة عبر `POST /api/auth/register`.
2. تسجل الدخول عبر `POST /api/auth/login` وتحصل على JWT صالح لثماني ساعات.
3. تنشر صاحبة المنزل طلباً عبر `POST /api/service-requests`.
4. تظهر العاملات المعتمدة فقط عبر `GET /api/worker-profiles`.
5. ينشأ الحجز عبر `POST /api/service-requests/{id}/bookings`؛ ورسوم الخدمة الحالية 10% من الأجر اليومي المتفق عليه.
