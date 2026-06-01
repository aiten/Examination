export interface StudentSubtask {
  id: number;
  subtaskId: number;
  description: string;
  points: number;
  bonus: boolean;
  seqNo: number;
  result: number | null;
  comment: string | null;
  commentPrivate: string | null;
}
