import { StudentExamResult } from './exam-result.model';

export interface StudentCourseResultQuery {
  firstName: string;
  lastName: string;
  pin: string;
  registrationCode: string;
}

export interface StudentCourseResult {
  courseName: string;
  studentName: string;
  studentExams: StudentExamResult[];
}
