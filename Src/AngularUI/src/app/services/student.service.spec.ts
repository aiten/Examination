import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { StudentService } from './student.service';
import { Student } from '../models/student.model';

describe('StudentService', () => {
  let service: StudentService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
    });
    service = TestBed.inject(StudentService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('getAll() should GET /api/student', () => {
    const mock: Student[] = [{ id: 1, firstName: 'Anna', lastName: 'Müller', classIds: [] }];
    service.getAll().subscribe(data => expect(data).toEqual(mock));
    http.expectOne('/api/student').flush(mock);
  });

  it('getById() should GET /api/student/:id', () => {
    const mock: Student = { id: 1, firstName: 'Anna', lastName: 'Müller', classIds: [2] };
    service.getById(1).subscribe(data => expect(data).toEqual(mock));
    http.expectOne('/api/student/1').flush(mock);
  });

  it('create() should POST /api/student', () => {
    const student: Student = { id: 0, firstName: 'Anna', lastName: 'Müller', classIds: [] };
    service.create(student).subscribe();
    const req = http.expectOne('/api/student');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(student);
    req.flush(student);
  });

  it('update() should PUT /api/student/:id', () => {
    const student: Student = { id: 1, firstName: 'Anna', lastName: 'Müller', classIds: [] };
    service.update(1, student).subscribe();
    const req = http.expectOne('/api/student/1');
    expect(req.request.method).toBe('PUT');
    req.flush(null);
  });

  it('delete() should DELETE /api/student/:id', () => {
    service.delete(1).subscribe();
    const req = http.expectOne('/api/student/1');
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });
});
