export interface StudentExamResultQuery {
  firstName: string;
  lastName: string;
  pin: string;
  registrationCode: string;
}

export interface StudentExamResultSubtask {
  seqNo: number;
  description: string;
  points: number;
  result: number | null;
  comment: string | null;
  bonus: boolean;
  date: string | null;
}

export interface StudentExamResult {
  examDescription: string;
  examType: number;
  examDate: string;
  studentName: string;
  subtasks: StudentExamResultSubtask[];
  totalPoints: number | null;
  percent: number | null;
  grade: number | null;
}
