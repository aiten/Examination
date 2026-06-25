import { Routes } from '@angular/router';
import { createAuthGuard } from 'keycloak-angular';
import { ForbiddenComponent } from './forbidden/forbidden.component';
import { environment } from '../environments/environment';
import { UserProfileComponent } from './user-profile/user-profile.component';
import { HomeComponent } from './home/home.component';

import { canActivateAuthRole } from './guards/auth-role.guard';
import { TeacherListComponent } from './teachers/teacher-list.component';
import { TeacherFormComponent } from './teachers/teacher-form.component';
import { ClassListComponent } from './classes/class-list.component';
import { ClassFormComponent } from './classes/class-form.component';
import { ExamListComponent } from './exams/exam-list.component';
import { ExamFormComponent } from './exams/exam-form.component';
import { StudentListComponent } from './students/student-list.component';
import { StudentFormComponent } from './students/student-form.component';
import { StudentImportComponent } from './students/student-import.component';
import { SubjectListComponent } from './subjects/subject-list.component';
import { SubjectFormComponent } from './subjects/subject-form.component';
import { CourseListComponent } from './courses/course-list.component';
import { CourseFormComponent } from './courses/course-form.component';
import { RegisterExamFormComponent } from './registration/register-exam-form.component';
import { RegisterCourseFormComponent } from './registration/register-course-form.component';
import { RegisterExamResultComponent } from './registration/register-exam-result.component';
import { RegisterCourseResultComponent } from './registration/register-course-result.component';
import { ResultQueryComponent } from './results/result-query.component';
import { ResultDisplayComponent } from './results/result-display.component';
import { SubtaskListComponent } from './subtasks/subtask-list.component';
import { StudentExamListComponent } from './student-exams/student-exam-list.component';
import { StudentExamFormComponent } from './student-exams/student-exam-form.component';
import { StudentExamSubtasksComponent } from './student-exams/student-exam-subtasks.component';
import { SubtaskStudentsComponent } from './subtasks/subtask-students.component';

export const routes: Routes = [
  { path: '', redirectTo: '/home', pathMatch: 'full' },
  { path: 'home', component: HomeComponent },  
  { path: 'teachers', component: TeacherListComponent , canActivate: [canActivateAuthRole],   data: { role: environment.roles.admin }, },
  { path: 'teachers/:id', component: TeacherFormComponent, canActivate: [canActivateAuthRole],   data: { role: environment.roles.admin },  },
  { path: 'classes', component: ClassListComponent, canActivate: [canActivateAuthRole],   data: { role: environment.roles.admin },  },
  { path: 'classes/:id', component: ClassFormComponent, canActivate: [canActivateAuthRole],   data: { role: environment.roles.admin },  },
  { path: 'exams', component: ExamListComponent , canActivate: [canActivateAuthRole],   data: { role: environment.roles.user },},
  { path: 'exams/:examId/subtasks', component: SubtaskListComponent , canActivate: [canActivateAuthRole],   data: { role: environment.roles.user },},
  { path: 'exams/:examId/subtasks/:subtaskId/students', component: SubtaskStudentsComponent, canActivate: [canActivateAuthRole], data: { role: environment.roles.user } },
  { path: 'exams/:examId/students', component: StudentExamListComponent , canActivate: [canActivateAuthRole],   data: { role: environment.roles.user },},
  { path: 'exams/:examId/students/:id/edit', component: StudentExamFormComponent, canActivate: [canActivateAuthRole], data: { role: environment.roles.user } },
  { path: 'exams/:examId/students/:id/subtasks', component: StudentExamSubtasksComponent , canActivate: [canActivateAuthRole],   data: { role: environment.roles.user },},
  { path: 'exams/:id', component: ExamFormComponent, canActivate: [canActivateAuthRole],   data: { role: environment.roles.user }, },
  { path: 'students', component: StudentListComponent, canActivate: [canActivateAuthRole],   data: { role: environment.roles.admin },  },
  { path: 'students/import', component: StudentImportComponent, canActivate: [canActivateAuthRole],   data: { role: environment.roles.admin },  },
  { path: 'students/:id', component: StudentFormComponent, canActivate: [canActivateAuthRole],   data: { role: environment.roles.admin },  },
  { path: 'subjects', component: SubjectListComponent, canActivate: [canActivateAuthRole],   data: { role: environment.roles.admin },  },
  { path: 'subjects/:id', component: SubjectFormComponent, canActivate: [canActivateAuthRole],   data: { role: environment.roles.admin },  },
  { path: 'courses', component: CourseListComponent, canActivate: [canActivateAuthRole],   data: { role: environment.roles.admin },  },
  { path: 'courses/:id', component: CourseFormComponent, canActivate: [canActivateAuthRole],   data: { role: environment.roles.admin },  },
  { path: 'registration/exam', component: RegisterExamFormComponent },
  { path: 'registration/course', component: RegisterCourseFormComponent },
  { path: 'registration/exam/result', component: RegisterExamResultComponent },
  { path: 'registration/exam/result', component: RegisterExamResultComponent },
  { path: 'registration/course/result', component: RegisterCourseResultComponent },
  { path: 'result', component: ResultQueryComponent },
  { path: 'result/display', component: ResultDisplayComponent },
  { path: 'profile', component: UserProfileComponent,  canActivate: [canActivateAuthRole],    data: { role: environment.roles.viewProfile }  },
  { path: 'forbidden', component: ForbiddenComponent },
  { path: '**', redirectTo: '/home' } 

];
