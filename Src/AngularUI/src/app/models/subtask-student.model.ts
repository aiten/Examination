export interface SubtaskStudent {
  id: number;
  studentExamId: number;
  lastName: string;
  firstName: string;
  result: number | null;
  comment: string | null;
  commentPrivate: string | null;
}

export interface SubtaskStudentCreate {
  studentExamId: number;
  result: number | null;
  comment: string | null;
  commentPrivate: string | null;
}
