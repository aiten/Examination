import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { StudentExamResultQuery, StudentExamResult } from '../models/exam-result.model';

@Injectable({ providedIn: 'root' })
export class ResultService {
  private readonly url = '/api/result';
  result = signal<StudentExamResult | null>(null);

  constructor(private http: HttpClient) {}

  getResult(query: StudentExamResultQuery): Observable<StudentExamResult> {
    return this.http.post<StudentExamResult>(this.url, query);
  }
}
