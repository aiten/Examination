export interface ExamRegistrationRequest {
  firstName: string;
  lastName: string;
  loginName: string;
  pin: number;
}

export interface ExamRegistrationResult {
  id: number;
  studentName: string;
  examDescription: string;
  examDate: string;
  registrationCode: string;
}
