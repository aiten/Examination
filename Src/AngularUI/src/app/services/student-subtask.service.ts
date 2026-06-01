import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { StudentSubtask } from '../models/student-subtask.model';

@Injectable({ providedIn: 'root' })
export class StudentSubtaskService {
  private url(examId: number, studentExamId: number) {
    return `/api/exam/${examId}/students/${studentExamId}/subtasks`;
  }

  constructor(private http: HttpClient) {}

  getAll(examId: number, studentExamId: number): Observable<StudentSubtask[]> {
    return this.http.get<StudentSubtask[]>(this.url(examId, studentExamId));
  }

  update(examId: number, studentExamId: number, subtask: StudentSubtask): Observable<void> {
    return this.http.put<void>(`${this.url(examId, studentExamId)}/${subtask.id}`, subtask);
  }

  create(examId: number, studentExamId: number, subtask: StudentSubtask): Observable<StudentSubtask> {
    return this.http.post<StudentSubtask>(`${this.url(examId, studentExamId)}`, subtask);
  }

}
  