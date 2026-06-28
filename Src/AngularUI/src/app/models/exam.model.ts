export interface Exam {
  id: number;
  description: string;
  examType: number;
  teacherId: number;
  courseId: number;
  date: string | null;
  from: string | null;
  to: string | null;
  pin: string | null;
  canRegister: boolean;
  canShowResults: boolean;
}
