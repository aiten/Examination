export interface ExamRegistrationRequest {
  firstName: string;
  lastName: string;
  loginName?: string | null;
  pin: string;
}

export interface ExamRegistrationResult {
  id: number;
  firstName: string;
  lastName: string;
  pin: string | null;
  examDescription: string;
  examDate: string;
  registrationCode: string;
}
