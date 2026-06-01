import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SchoolClass } from '../models/class.model';

@Injectable({ providedIn: 'root' })
export class ClassService {
  private readonly url = '/api/class';

  constructor(private http: HttpClient) {}

  getAll(): Observable<SchoolClass[]> {
    return this.http.get<SchoolClass[]>(this.url);
  }

  getById(id: number): Observable<SchoolClass> {
    return this.http.get<SchoolClass>(`${this.url}/${id}`);
  }

  create(schoolClass: SchoolClass): Observable<SchoolClass> {
    return this.http.post<SchoolClass>(this.url, schoolClass);
  }

  update(id: number, schoolClass: SchoolClass): Observable<void> {
    return this.http.put<void>(`${this.url}/${id}`, schoolClass);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
