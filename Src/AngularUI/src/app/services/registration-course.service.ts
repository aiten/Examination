import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { RegistrationCourseRequest, RegistrationCourseResult } from '../models/registration.model';

@Injectable({ providedIn: 'root' })
export class RegistrationCourseService {
  private readonly url = '/api/registration/course';
  result = signal<RegistrationCourseResult | null>(null);

  constructor(private http: HttpClient) {}

  register(req: RegistrationCourseRequest): Observable<RegistrationCourseResult> {
    return this.http.post<RegistrationCourseResult>(this.url, req);
  }
}
