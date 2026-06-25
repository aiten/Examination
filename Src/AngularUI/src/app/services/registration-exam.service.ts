import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { RegistrationExamRequest, RegistrationExamResult } from '../models/registration.model';

@Injectable({ providedIn: 'root' })
export class RegistrationExamService {
  private readonly url = '/api/registration';
  result = signal<RegistrationExamResult | null>(null);

  constructor(private http: HttpClient) {}

  register(req: RegistrationExamRequest): Observable<RegistrationExamResult> {
    return this.http.post<RegistrationExamResult>(this.url, req);
  }
}
