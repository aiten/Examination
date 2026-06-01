export interface StudentExamOverview {
  id: number;
  studentId: number;
  firstName: string;
  lastName: string;
  loginName: string;
  registrationCode: string;
  countRated: number;
  points: number|null;
  percent: number|null;
  grade: number|null;
}

export interface StudentSubtaskResult {
  subtaskId: number;
  description: string;
  points: number;
  result: number;
}

export interface GradeSummary {
  grade: number;
  count: number;
}

export interface StudentExamDetail {
  id: number;
  examId: number;
  studentId: number;
  firstName: string;
  lastName: string;
  loginName: string;
  registrationCode: string;
  subtasks: StudentSubtaskResult[];
}

export interface StudentExamEdit {
  id: number;
  loginName: string;
  registrationCode: string;
}
