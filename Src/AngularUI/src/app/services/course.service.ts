import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Course } from '../models/course.model';

@Injectable({ providedIn: 'root' })
export class CourseService {
  private readonly url = '/api/course';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Course[]> {
    return this.http.get<Course[]>(this.url);
  }

  getById(id: number): Observable<Course> {
    return this.http.get<Course>(`${this.url}/${id}`);
  }

  create(course: Course): Observable<Course> {
    return this.http.post<Course>(this.url, course);
  }

  update(id: number, course: Course): Observable<void> {
    return this.http.put<void>(`${this.url}/${id}`, course);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
