export interface StudentCourseResultQuery {
  firstName: string;
  lastName: string;
  pin: string;
  registrationName: string;
}

export interface StudentCourseResultSubtask {
  seqNo: number;
  description: string;
  points: number;
  result: number | null;
  comment: string | null;
  bonus: boolean;
}

export interface StudentCourseResult {
  courseDescription: string;
  courseDate: string;
  studentName: string;
  subtasks: StudentCourseResultSubtask[];
  totalPoints: number | null;
  percent: number | null;
  grade: number | null;
}
