import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ConfigDto {
  currentSchoolYear: number;
}

@Injectable({ providedIn: 'root' })
export class ConfigService {
  private readonly url = '/api/config';

  constructor(private http: HttpClient) {}

  getConfig(): Observable<ConfigDto> {
    return this.http.get<ConfigDto>(this.url);
  }
}
