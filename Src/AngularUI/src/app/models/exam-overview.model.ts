export interface ExamOverview {
  id: number;
  description: string;
  teacher: string;
  course: string;
  courseYear: number;
  date: string | null;
  from: string | null;
  to: string | null;
  pin?: number | null;
  subtask: string[];
  students: string[];
}
