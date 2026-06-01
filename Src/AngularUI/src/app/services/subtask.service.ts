import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Subtask } from '../models/subtask.model';

@Injectable({ providedIn: 'root' })
export class SubtaskService {
  private url(examId: number) { return `/api/exam/${examId}/subtask`; }

  constructor(private http: HttpClient) {}

  getAll(examId: number): Observable<Subtask[]> {
    return this.http.get<Subtask[]>(this.url(examId));
  }

  create(examId: number, subtask: Subtask): Observable<Subtask> {
    return this.http.post<Subtask>(this.url(examId), subtask);
  }

  update(examId: number, id: number, subtask: Subtask): Observable<void> {
    return this.http.put<void>(`${this.url(examId)}/${id}`, subtask);
  }

  delete(examId: number, id: number): Observable<void> {
    return this.http.delete<void>(`${this.url(examId)}/${id}`);
  }
}
