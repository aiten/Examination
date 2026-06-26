import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { StudentCourseResultQuery, StudentCourseResult } from '../models/course-result.model';

@Injectable({ providedIn: 'root' })
export class CourseResultService {
  private readonly url = '/api/courseresult';
  result = signal<StudentCourseResult | null>(null);

  constructor(private http: HttpClient) {}

  getResult(query: StudentCourseResultQuery): Observable<StudentCourseResult> {
    return this.http.post<StudentCourseResult>(this.url, query);
  }
}
