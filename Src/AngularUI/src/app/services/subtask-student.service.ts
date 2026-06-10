import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SubtaskStudent, SubtaskStudentCreate } from '../models/subtask-student.model';

@Injectable({ providedIn: 'root' })
export class SubtaskStudentService {
  private url(examId: number, subtaskId: number) {
    return `/api/exam/${examId}/subtasks/${subtaskId}/students`;
  }

  constructor(private http: HttpClient) {}

  getAll(examId: number, subtaskId: number): Observable<SubtaskStudent[]> {
    return this.http.get<SubtaskStudent[]>(this.url(examId, subtaskId));
  }

  create(examId: number, subtaskId: number, dto: SubtaskStudentCreate): Observable<SubtaskStudent> {
    return this.http.post<SubtaskStudent>(this.url(examId, subtaskId), dto);
  }

  update(examId: number, subtaskId: number, entry: SubtaskStudent): Observable<void> {
    return this.http.put<void>(`${this.url(examId, subtaskId)}/${entry.id}`, entry);
  }
}
