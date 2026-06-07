export interface ExamRegistrationRequest {
  firstName: string;
  lastName: string;
  loginName: string;
  pin: number;
}

export interface ExamRegistrationResult {
  id: number;
  firstName: string;
  lastName: string;
  pin: number;
  examDescription: string;
  examDate: string;
  registrationCode: string;
}
