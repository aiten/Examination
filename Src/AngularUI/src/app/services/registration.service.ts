import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ExamRegistrationRequest, ExamRegistrationResult } from '../models/registration.model';

@Injectable({ providedIn: 'root' })
export class RegistrationService {
  private readonly url = '/api/registration';
  result = signal<ExamRegistrationResult | null>(null);

  constructor(private http: HttpClient) {}

  register(req: ExamRegistrationRequest): Observable<ExamRegistrationResult> {
    return this.http.post<ExamRegistrationResult>(this.url, req);
  }
}
