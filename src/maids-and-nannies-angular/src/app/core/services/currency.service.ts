import { Injectable, Signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface CurrencyDto {
  id: number;
  code: string;
  symbol: string;
  nameAr: string;
  nameEn: string;
  rateToEgp: number;
  isActive: boolean;
}

@Injectable({ providedIn: 'root' })
export class CurrencyService {
  private readonly API_URL = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getCurrencies(includeInactive = false): Observable<CurrencyDto[]> {
    return this.http.get<CurrencyDto[]>(`${this.API_URL}/currencies?includeInactive=${includeInactive}`);
  }

  createCurrency(data: any): Observable<any> {
    return this.http.post(`${this.API_URL}/currencies`, data);
  }

  updateCurrency(id: number, data: any): Observable<any> {
    return this.http.put(`${this.API_URL}/currencies/${id}`, data);
  }

  deleteCurrency(id: number): Observable<any> {
    return this.http.delete(`${this.API_URL}/currencies/${id}`);
  }

      loadCurrencies(currenciesMap: any) {
        this.getCurrencies().subscribe({
            next: (data) => {
                const map: { [id: number]: string } = {};
                data.forEach(c => { map[c.id] = c.code; });
                currenciesMap.set(map);
            }
        });
    }
}