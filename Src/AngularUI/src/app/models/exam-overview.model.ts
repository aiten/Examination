export interface ExamOverview {
  id: number;
  description: string;
  teacher: string;
  course: string;
  courseYear: number;
  date: string;
  from: string;
  to: string;
  pin?: number | null;
  subtask: string[];
  students: string[];
}
