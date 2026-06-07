export interface StudentExamResultQuery {
  firstName: string;
  lastName: string;
  pin: number;
  registrationCode: string;
}

export interface StudentExamResultSubtask {
  seqNo: number;
  description: string;
  points: number;
  result: number | null;
  comment: string | null;
  bonus: boolean;
}

export interface StudentExamResult {
  examDescription: string;
  examDate: string;
  studentName: string;
  subtasks: StudentExamResultSubtask[];
  totalPoints: number | null;
  percent: number | null;
  grade: number | null;
}
