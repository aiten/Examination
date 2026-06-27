export interface Exam {
  id: number;
  description: string;
  examType: number;
  teacherId: number;
  courseId: number;
  date: string;
  from: string;
  to: string;
  pin: string | null;
  canRegister: boolean;
  canShowResults: boolean;
}
