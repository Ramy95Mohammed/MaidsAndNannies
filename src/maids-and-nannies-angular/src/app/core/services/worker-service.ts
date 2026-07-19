import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { WorkerProfile } from '../interfaces/worker-profile';

@Injectable({
  providedIn: 'root'
})
export class WorkerService {
   private readonly API_URL = environment.apiUrl;

  constructor(private http: HttpClient) {}
 
  
   getWorkerProfile(): Observable<WorkerProfile> {
      return this.http.get<WorkerProfile>(`${this.API_URL}/worker/profile`);
    }

     updateWorkerProfile(data: any): Observable<WorkerProfile> {
    return this.http.put<WorkerProfile>(`${this.API_URL}/worker/profile`, data);
  }
}
