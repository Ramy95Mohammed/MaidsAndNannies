import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { State } from '../interfaces/state';
import { Country } from '../interfaces/country';
import { City } from '../interfaces/city';

@Injectable({
  providedIn: 'root'
})
export class GlobalizationSpecsService {
  private readonly API_URL = environment.apiUrl;
  
  constructor(private http: HttpClient) {}
    getCountries(): Observable<Country[]> {
      return this.http.get<Country[]>(`${this.API_URL}/GlobalizationSpecs`);
    }

    getStatesByCountryId(countryId:number): Observable<State[]>{
      return this.http.get<State[]>(`${this.API_URL}/GlobalizationSpecs/${countryId}`);
    }

    getCitiesByStateId(stateId:number):Observable<City[]>{
      return this.http.get<City[]>(`${this.API_URL}/GlobalizationSpecs/stats/${stateId}`);
    }
}
