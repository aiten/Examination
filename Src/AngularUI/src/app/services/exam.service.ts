import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Exam } from '../models/exam.model';
import { ExamOverview } from '../models/exam-overview.model';

@Injectable({ providedIn: 'root' })
export class ExamService {
  private readonly url = '/api/exam';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Exam[]> {
    return this.http.get<Exam[]>(this.url);
  }

  getOverview(): Observable<ExamOverview[]> {
    return this.http.get<ExamOverview[]>(`${this.url}/overview`);
  }

  getById(id: number): Observable<Exam> {
    return this.http.get<Exam>(`${this.url}/${id}`);
  }

  create(exam: Exam): Observable<Exam> {
    return this.http.post<Exam>(this.url, exam);
  }

  update(id: number, exam: Exam): Observable<void> {
    return this.http.put<void>(`${this.url}/${id}`, exam);
  }
}
