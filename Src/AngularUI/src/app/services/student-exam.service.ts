import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { GradeSummary, StudentExamDetail, StudentExamEdit, StudentExamOverview, StudentSubtaskResult } from '../models/student-exam.model';

@Injectable({ providedIn: 'root' })
export class StudentExamService {
  private url(examId: number) { return `/api/exam/${examId}/students`; }

  constructor(private http: HttpClient) {}

  getAll(examId: number): Observable<StudentExamOverview[]> {
    return this.http.get<StudentExamOverview[]>(this.url(examId));
  }

  getById(examId: number, id: number): Observable<StudentExamDetail> {
    return this.http.get<StudentExamDetail>(`${this.url(examId)}/${id}`);
  }

  getSummary(examId: number): Observable<GradeSummary[]> {
    return this.http.get<GradeSummary[]>(`${this.url(examId)}/summary`);
  }

  update(examId: number, id: number, subtasks: Pick<StudentSubtaskResult, 'subtaskId' | 'result'>[]): Observable<void> {
    return this.http.put<void>(`${this.url(examId)}/${id}`, subtasks);
  }

  updateStudentExam(examId: number, id: number, data: StudentExamEdit): Observable<void> {
    return this.http.put<void>(`${this.url(examId)}/${id}`, data);
  }

  delete(examId: number, id: number): Observable<void> {
    return this.http.delete<void>(`${this.url(examId)}/${id}`);
  }
}
