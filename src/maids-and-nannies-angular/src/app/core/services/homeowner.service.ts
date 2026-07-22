import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface HomeownerProfile {
  id: number;
  userId: string;
  fullName: string;
  email: string | null;
  phoneNumber: string | null;
  whatsAppNumber:string;
  nationalIdNumber: string;
  nationalIdImageUrl: string | null;
  selfieImageUrl: string | null;
  proofOfAddressImageUrl: string | null;
  address: string;
  state:string;
  city: string;
  district: string | null;
  verificationStatus: number;
  verificationNotes: string | null;
  verifiedAt: string | null;
}

@Injectable({ providedIn: 'root' })
export class HomeownerService {
  private readonly API_URL = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getProfile(): Observable<HomeownerProfile> {
    return this.http.get<HomeownerProfile>(`${this.API_URL}/homeowner/profile`);
  }

  updateProfile(formData: FormData): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.API_URL}/homeowner/profile`, formData);
  }
}