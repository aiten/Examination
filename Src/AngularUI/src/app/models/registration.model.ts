export interface RegistrationExamRequest {
  firstName: string;
  lastName: string;
  loginName?: string | null;
  pin: string;
}

export interface RegistrationExamResult {
  id: number;
  firstName: string;
  lastName: string;
  pin: string | null;
  examDescription: string;
  examDate: string;
  registrationCode: string;
}

export interface RegistrationCourseRequest {
  firstName: string;
  lastName: string;
  pin: string;
}

export interface RegistrationCourseResult {
  id: number;
  firstName: string;
  lastName: string;
  pin: string | null;
  courseDescription: string;
  registrationCode: string;
}
