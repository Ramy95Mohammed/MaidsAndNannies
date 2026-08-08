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
    return this.http.post(`${this.API_URL}/worker`,filters);
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

  getBookingCalculationInfo(data: any): Observable<any> {
    return this.http.post(`${this.API_URL}/booking/bookingCalculationInfo`, data);
  }

  getMyBookings(): Observable<any> {
        return this.http.get(`${this.API_URL}/booking`);
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

   getAllBookings(params?: any): Observable<any> {
    return this.http.get(`${this.API_URL}/admin/bookings`, { params });
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
    return this.http.post(`${this.API_URL}/admin/homeowners/${id}/approve`, {});
}

rejectHomeowner(id: number, reason: string): Observable<any> {
    return this.http.post(`${this.API_URL}/admin/homeowners/${id}/reject`, { reason });
}

getPendingWorkers(): Observable<any> {
    return this.http.get(`${this.API_URL}/admin/workers/pending`);
}

verifyWorker(id: number): Observable<any> {
    return this.http.post(`${this.API_URL}/admin/workers/${id}/approve`, {});
}

getAllWorkers(): Observable<any> {
  return this.http.get(`${this.API_URL}/admin/workers`);
}
updateWorkerAvailability(id: number, isAvailable: boolean): Observable<any> {
  return this.http.put(`${this.API_URL}/admin/workers/${id}/availability`, { isAvailable });
}

getPendingPayments(): Observable<any> {
    return this.http.get(`${this.API_URL}/admin/payments/pending`);
}

confirmPayment(id: number): Observable<any> {
    return this.http.post(`${this.API_URL}/admin/payments/${id}/confirm`, {});
}

rejectPayment(id: number, reason?: string): Observable<any> {
    return this.http.post(`${this.API_URL}/admin/payments/${id}/reject`, { reason });
}
  
  adminRegisterHomeowner(data: any): Observable<any> {
    return this.http.post(`${this.API_URL}/admin/homeowners/register`, data);
  }

    getAllHomeowners(): Observable<any> {
    return this.http.get(`${this.API_URL}/admin/homeowners`);
  }

  updateHomeownerReplacementLimits(id: number, data: { maxFaultReplacementCount: number | null; maxPreferenceReplacementCount: number | null }): Observable<any> {
    return this.http.put(`${this.API_URL}/admin/homeowners/${id}/replacement-limits`, data);
  }

    // Password Reset (admin)
  getPasswordResetRequests(): Observable<any> {
    return this.http.get(`${this.API_URL}/admin/password-reset`);
  }

  markResetRequestSent(id: number): Observable<any> {
    return this.http.post(`${this.API_URL}/admin/password-reset/${id}/mark-sent`, {});
  }


  // ── Job Posts ──
createJobPost(data: any): Observable<any> {
  return this.http.post(`${this.API_URL}/jobposts`, data);
}

updateJobPost(id: number, data: any): Observable<any> {
  return this.http.put(`${this.API_URL}/jobposts/${id}`, data);
}
deleteJobPost(id: number): Observable<any> {
  return this.http.delete(`${this.API_URL}/jobposts/${id}`);
}

getJobPostCalculationInfo(data: any): Observable<any> {
  return this.http.post(`${this.API_URL}/jobposts/jobPostCalculationInfo`, data);
}



getMyJobPosts(): Observable<any> {
  return this.http.get(`${this.API_URL}/jobposts/my`);
}
getJobPostById(id: number): Observable<any> {
  return this.http.get(`${this.API_URL}/jobposts/${id}`);
}
getApprovedJobPosts(params?: any): Observable<any> {
  return this.http.get(`${this.API_URL}/jobposts`, { params });
}
applyForJob(postId: number, message?: string): Observable<any> {
  return this.http.post(`${this.API_URL}/jobposts/${postId}/apply`, { message });
}
getJobApplications(postId: number): Observable<any> {
  return this.http.get(`${this.API_URL}/jobposts/${postId}/applications`);
}
acceptApplication(postId: number, appId: number): Observable<any> {
  return this.http.post(`${this.API_URL}/jobposts/${postId}/applications/${appId}/accept`, {});
}
getMyApplications(): Observable<any> {
  return this.http.get(`${this.API_URL}/jobposts/my-applications`);
}
getPendingJobPosts(): Observable<any> {
  return this.http.get(`${this.API_URL}/adminjobposts/pending`);
}
reviewJobPost(id: number, data: any): Observable<any> {
  return this.http.put(`${this.API_URL}/adminjobposts/${id}/review`, data);
}

  // Policies
  getPolicies(): Observable<any> {
    return this.http.get(`${this.API_URL}/policies`);
  }

  updatePolicy(key: string, data: any): Observable<any> {
    return this.http.put(`${this.API_URL}/policies/${key}`, data);
  }

 // Settings
  getSettings(): Observable<any> {
    return this.http.get(`${this.API_URL}/adminsettings`);
  }

  getSettingByKey(key:string): Observable<any> {
    return this.http.get(`${this.API_URL}/adminsettings/`+ key);
  }


  updateSettings(settings: any[]): Observable<any> {
    return this.http.put(`${this.API_URL}/adminsettings`, settings);
  }

  // Backup
    createBackup() {
        return this.http.get(`${this.API_URL}/admin/backup/create`, { responseType: 'blob' });
  }
  restoreBackup(file: File) {
        const fd = new FormData();
        fd.append('file', file);
        return this.http.post(`${this.API_URL}/admin/backup/restore`, fd);
  }
}
