import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { StudentCourse } from '../models/student-course.model';

@Injectable({ providedIn: 'root' })
export class StudentCourseService {
  private url(courseId: number) { return `/api/course/${courseId}/students`; }

  constructor(private http: HttpClient) {}

  getAll(courseId: number): Observable<StudentCourse[]> {
    return this.http.get<StudentCourse[]>(this.url(courseId));
  }
}
