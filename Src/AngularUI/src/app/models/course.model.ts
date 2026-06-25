export interface Course {
  id: number;
  name: string;
  year: number;
  subjectId: number;
  classIds: number[];
  teacherIds: number[];
  canRegister: boolean;
  pin: string | null;
}
