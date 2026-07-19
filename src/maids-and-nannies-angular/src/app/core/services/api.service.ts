import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private readonly API_URL = environment.apiUrl;

  constructor(private http: HttpClient) {}

  // Workers
  getWorkers(filters?: any): Observable<any> {
    let params = new HttpParams();
    if (filters) {
      Object.keys(filters).forEach(key => {
        if (filters[key] !== null && filters[key] !== undefined && filters[key] !== '') {
          params = params.set(key, filters[key]);
        }
      });
    }
    return this.http.get(`${this.API_URL}/worker`, { params });
  }

  getWorker(id: string | number): Observable<any> {
    return this.http.get(`${this.API_URL}/worker/${id}`);
  }

  getWorkerProfile(): Observable<any> {
    return this.http.get(`${this.API_URL}/worker/profile`);
  }

  updateWorkerProfile(data: any): Observable<any> {
    return this.http.put(`${this.API_URL}/worker/profile`, data);
  }

  // Bookings
  createBooking(data: any): Observable<any> {
    return this.http.post(`${this.API_URL}/booking`, data);
  }

  getMyBookings(): Observable<any> {
    return this.http.get(`${this.API_URL}/booking/my`);
  }

  getWorkerBookings(): Observable<any> {
    return this.http.get(`${this.API_URL}/worker/bookings`);
  }

  getBooking(id: string | number): Observable<any> {
    return this.http.get(`${this.API_URL}/booking/${id}`);
  }

  updateBookingStatus(id: number, status: number): Observable<any> {
    return this.http.put(`${this.API_URL}/booking/${id}/status`, { status });
  }

  acceptBooking(id: number): Observable<any> {
    return this.http.put(`${this.API_URL}/booking/${id}/accept`, {});
  }

  rejectBooking(id: number): Observable<any> {
    return this.http.put(`${this.API_URL}/booking/${id}/reject`, {});
  }

  startBooking(id: number): Observable<any> {
    return this.http.put(`${this.API_URL}/booking/${id}/start`, {});
  }

  completeBooking(id: number): Observable<any> {
    return this.http.put(`${this.API_URL}/booking/${id}/complete`, {});
  }

  // Reviews
  createReview(data: any): Observable<any> {
    return this.http.post(`${this.API_URL}/review`, data);
  }

  getWorkerReviews(workerId: number): Observable<any> {
    return this.http.get(`${this.API_URL}/review/worker/${workerId}`);
  }

  // Payments
  uploadPaymentProof(bookingId: number, data: any): Observable<any> {
    return this.http.post(`${this.API_URL}/payment/proof/${bookingId}`, data);
  }

  // Messages
  sendMessage(data: any): Observable<any> {
    return this.http.post(`${this.API_URL}/message`, data);
  }

  getConversations(): Observable<any> {
    return this.http.get(`${this.API_URL}/message/conversations`);
  }

  getMessages(userId: string): Observable<any> {
    return this.http.get(`${this.API_URL}/message/${userId}`);
  }

  // Admin
  getAdminDashboard(): Observable<any> {
    return this.http.get(`${this.API_URL}/admin/dashboard`);
  }

  getPendingHomeowners(): Observable<any> {
    return this.http.get(`${this.API_URL}/admin/homeowners/pending`);
  }

  verifyHomeowner(id: number): Observable<any> {
    return this.http.put(`${this.API_URL}/admin/homeowners/${id}/verify`, {});
  }

  rejectHomeowner(id: number, reason: string): Observable<any> {
    return this.http.put(`${this.API_URL}/admin/homeowners/${id}/reject`, { reason });
  }

  getPendingWorkers(): Observable<any> {
    return this.http.get(`${this.API_URL}/admin/workers/pending`);
  }

  verifyWorker(id: number): Observable<any> {
    return this.http.put(`${this.API_URL}/admin/workers/${id}/verify`, {});
  }

  getUsers(role?: string, page: number = 1): Observable<any> {
    let params = new HttpParams().set('page', page);
    if (role) params = params.set('role', role);
    return this.http.get(`${this.API_URL}/admin/users`, { params });
  }

  toggleUser(id: string): Observable<any> {
    return this.http.put(`${this.API_URL}/admin/users/${id}/toggle`, {});
  }

  getPendingPayments(): Observable<any> {
    return this.http.get(`${this.API_URL}/payment/pending`);
  }

  confirmPayment(id: number): Observable<any> {
    return this.http.put(`${this.API_URL}/payment/${id}/confirm`, {});
  }

  rejectPayment(id: number, reason?: string): Observable<any> {
    return this.http.put(`${this.API_URL}/payment/${id}/reject`, { reason });
  }
}
