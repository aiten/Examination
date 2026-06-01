using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class V3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassCourse_Classes_ClassesId",
                table: "ClassCourse");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassCourse_Courses_CoursesId",
                table: "ClassCourse");

            migrationBuilder.DropForeignKey(
                name: "FK_Classes_Teachers_TeacherId",
                table: "Classes");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Subjects_SubjectId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseTeacher_Courses_CoursesId",
                table: "CourseTeacher");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseTeacher_Teachers_TeachersId",
                table: "CourseTeacher");

            migrationBuilder.DropForeignKey(
                name: "FK_Exams_Courses_CourseId",
                table: "Exams");

            migrationBuilder.DropForeignKey(
                name: "FK_Exams_Teachers_TeacherId",
                table: "Exams");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentClass_Classes_ClassesId",
                table: "StudentClass");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentClass_Students_StudentsId",
                table: "StudentClass");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentExams_Exams_ExamId",
                table: "StudentExams");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentExams_Students_StudentId",
                table: "StudentExams");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentSubtasks_StudentExams_StudentExamId",
                table: "StudentSubtasks");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentSubtasks_Subtasks_SubtaskId",
                table: "StudentSubtasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Subtasks_Exams_ExamId",
                table: "Subtasks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Teachers",
                table: "Teachers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Subtasks",
                table: "Subtasks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Subjects",
                table: "Subjects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentSubtasks",
                table: "StudentSubtasks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Students",
                table: "Students");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentExams",
                table: "StudentExams");

            migrationBuilder.DropIndex(
                name: "IX_StudentExams_StudentId",
                table: "StudentExams");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Exams",
                table: "Exams");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Courses",
                table: "Courses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Classes",
                table: "Classes");

            migrationBuilder.RenameTable(
                name: "Teachers",
                newName: "Teacher");

            migrationBuilder.RenameTable(
                name: "Subtasks",
                newName: "Subtask");

            migrationBuilder.RenameTable(
                name: "Subjects",
                newName: "Subject");

            migrationBuilder.RenameTable(
                name: "StudentSubtasks",
                newName: "StudentSubtask");

            migrationBuilder.RenameTable(
                name: "Students",
                newName: "Student");

            migrationBuilder.RenameTable(
                name: "StudentExams",
                newName: "StudentExam");

            migrationBuilder.RenameTable(
                name: "Exams",
                newName: "Exam");

            migrationBuilder.RenameTable(
                name: "Courses",
                newName: "Course");

            migrationBuilder.RenameTable(
                name: "Classes",
                newName: "Class");

            migrationBuilder.RenameIndex(
                name: "IX_Teachers_LastName_FirstName",
                table: "Teacher",
                newName: "IX_Teacher_LastName_FirstName");

            migrationBuilder.RenameIndex(
                name: "IX_Subtasks_ExamId",
                table: "Subtask",
                newName: "IX_Subtask_ExamId");

            migrationBuilder.RenameIndex(
                name: "IX_StudentSubtasks_SubtaskId",
                table: "StudentSubtask",
                newName: "IX_StudentSubtask_SubtaskId");

            migrationBuilder.RenameIndex(
                name: "IX_StudentSubtasks_StudentExamId_SubtaskId",
                table: "StudentSubtask",
                newName: "IX_StudentSubtask_StudentExamId_SubtaskId");

            migrationBuilder.RenameIndex(
                name: "IX_StudentExams_RegistrationCode_ExamId",
                table: "StudentExam",
                newName: "IX_StudentExam_RegistrationCode_ExamId");

            migrationBuilder.RenameIndex(
                name: "IX_StudentExams_ExamId",
                table: "StudentExam",
                newName: "IX_StudentExam_ExamId");

            migrationBuilder.RenameIndex(
                name: "IX_Exams_TeacherId",
                table: "Exam",
                newName: "IX_Exam_TeacherId");

            migrationBuilder.RenameIndex(
                name: "IX_Exams_CourseId",
                table: "Exam",
                newName: "IX_Exam_CourseId");

            migrationBuilder.RenameIndex(
                name: "IX_Courses_SubjectId",
                table: "Course",
                newName: "IX_Course_SubjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Classes_TeacherId",
                table: "Class",
                newName: "IX_Class_TeacherId");

            migrationBuilder.RenameIndex(
                name: "IX_Classes_Description_Year",
                table: "Class",
                newName: "IX_Class_Description_Year");

            migrationBuilder.AddColumn<string>(
                name: "CommentPrivate",
                table: "StudentSubtask",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Teacher",
                table: "Teacher",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Subtask",
                table: "Subtask",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Subject",
                table: "Subject",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentSubtask",
                table: "StudentSubtask",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Student",
                table: "Student",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentExam",
                table: "StudentExam",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Exam",
                table: "Exam",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Course",
                table: "Course",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Class",
                table: "Class",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Student_LastName_FirstName",
                table: "Student",
                columns: new[] { "LastName", "FirstName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentExam_StudentId_ExamId",
                table: "StudentExam",
                columns: new[] { "StudentId", "ExamId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Class_Teacher_TeacherId",
                table: "Class",
                column: "TeacherId",
                principalTable: "Teacher",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassCourse_Class_ClassesId",
                table: "ClassCourse",
                column: "ClassesId",
                principalTable: "Class",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassCourse_Course_CoursesId",
                table: "ClassCourse",
                column: "CoursesId",
                principalTable: "Course",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Course_Subject_SubjectId",
                table: "Course",
                column: "SubjectId",
                principalTable: "Subject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseTeacher_Course_CoursesId",
                table: "CourseTeacher",
                column: "CoursesId",
                principalTable: "Course",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseTeacher_Teacher_TeachersId",
                table: "CourseTeacher",
                column: "TeachersId",
                principalTable: "Teacher",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Exam_Course_CourseId",
                table: "Exam",
                column: "CourseId",
                principalTable: "Course",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Exam_Teacher_TeacherId",
                table: "Exam",
                column: "TeacherId",
                principalTable: "Teacher",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentClass_Class_ClassesId",
                table: "StudentClass",
                column: "ClassesId",
                principalTable: "Class",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentClass_Student_StudentsId",
                table: "StudentClass",
                column: "StudentsId",
                principalTable: "Student",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentExam_Exam_ExamId",
                table: "StudentExam",
                column: "ExamId",
                principalTable: "Exam",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentExam_Student_StudentId",
                table: "StudentExam",
                column: "StudentId",
                principalTable: "Student",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentSubtask_StudentExam_StudentExamId",
                table: "StudentSubtask",
                column: "StudentExamId",
                principalTable: "StudentExam",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentSubtask_Subtask_SubtaskId",
                table: "StudentSubtask",
                column: "SubtaskId",
                principalTable: "Subtask",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Subtask_Exam_ExamId",
                table: "Subtask",
                column: "ExamId",
                principalTable: "Exam",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Class_Teacher_TeacherId",
                table: "Class");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassCourse_Class_ClassesId",
                table: "ClassCourse");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassCourse_Course_CoursesId",
                table: "ClassCourse");

            migrationBuilder.DropForeignKey(
                name: "FK_Course_Subject_SubjectId",
                table: "Course");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseTeacher_Course_CoursesId",
                table: "CourseTeacher");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseTeacher_Teacher_TeachersId",
                table: "CourseTeacher");

            migrationBuilder.DropForeignKey(
                name: "FK_Exam_Course_CourseId",
                table: "Exam");

            migrationBuilder.DropForeignKey(
                name: "FK_Exam_Teacher_TeacherId",
                table: "Exam");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentClass_Class_ClassesId",
                table: "StudentClass");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentClass_Student_StudentsId",
                table: "StudentClass");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentExam_Exam_ExamId",
                table: "StudentExam");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentExam_Student_StudentId",
                table: "StudentExam");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentSubtask_StudentExam_StudentExamId",
                table: "StudentSubtask");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentSubtask_Subtask_SubtaskId",
                table: "StudentSubtask");

            migrationBuilder.DropForeignKey(
                name: "FK_Subtask_Exam_ExamId",
                table: "Subtask");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Teacher",
                table: "Teacher");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Subtask",
                table: "Subtask");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Subject",
                table: "Subject");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentSubtask",
                table: "StudentSubtask");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentExam",
                table: "StudentExam");

            migrationBuilder.DropIndex(
                name: "IX_StudentExam_StudentId_ExamId",
                table: "StudentExam");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Student",
                table: "Student");

            migrationBuilder.DropIndex(
                name: "IX_Student_LastName_FirstName",
                table: "Student");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Exam",
                table: "Exam");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Course",
                table: "Course");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Class",
                table: "Class");

            migrationBuilder.DropColumn(
                name: "CommentPrivate",
                table: "StudentSubtask");

            migrationBuilder.RenameTable(
                name: "Teacher",
                newName: "Teachers");

            migrationBuilder.RenameTable(
                name: "Subtask",
                newName: "Subtasks");

            migrationBuilder.RenameTable(
                name: "Subject",
                newName: "Subjects");

            migrationBuilder.RenameTable(
                name: "StudentSubtask",
                newName: "StudentSubtasks");

            migrationBuilder.RenameTable(
                name: "StudentExam",
                newName: "StudentExams");

            migrationBuilder.RenameTable(
                name: "Student",
                newName: "Students");

            migrationBuilder.RenameTable(
                name: "Exam",
                newName: "Exams");

            migrationBuilder.RenameTable(
                name: "Course",
                newName: "Courses");

            migrationBuilder.RenameTable(
                name: "Class",
                newName: "Classes");

            migrationBuilder.RenameIndex(
                name: "IX_Teacher_LastName_FirstName",
                table: "Teachers",
                newName: "IX_Teachers_LastName_FirstName");

            migrationBuilder.RenameIndex(
                name: "IX_Subtask_ExamId",
                table: "Subtasks",
                newName: "IX_Subtasks_ExamId");

            migrationBuilder.RenameIndex(
                name: "IX_StudentSubtask_SubtaskId",
                table: "StudentSubtasks",
                newName: "IX_StudentSubtasks_SubtaskId");

            migrationBuilder.RenameIndex(
                name: "IX_StudentSubtask_StudentExamId_SubtaskId",
                table: "StudentSubtasks",
                newName: "IX_StudentSubtasks_StudentExamId_SubtaskId");

            migrationBuilder.RenameIndex(
                name: "IX_StudentExam_RegistrationCode_ExamId",
                table: "StudentExams",
                newName: "IX_StudentExams_RegistrationCode_ExamId");

            migrationBuilder.RenameIndex(
                name: "IX_StudentExam_ExamId",
                table: "StudentExams",
                newName: "IX_StudentExams_ExamId");

            migrationBuilder.RenameIndex(
                name: "IX_Exam_TeacherId",
                table: "Exams",
                newName: "IX_Exams_TeacherId");

            migrationBuilder.RenameIndex(
                name: "IX_Exam_CourseId",
                table: "Exams",
                newName: "IX_Exams_CourseId");

            migrationBuilder.RenameIndex(
                name: "IX_Course_SubjectId",
                table: "Courses",
                newName: "IX_Courses_SubjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Class_TeacherId",
                table: "Classes",
                newName: "IX_Classes_TeacherId");

            migrationBuilder.RenameIndex(
                name: "IX_Class_Description_Year",
                table: "Classes",
                newName: "IX_Classes_Description_Year");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Teachers",
                table: "Teachers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Subtasks",
                table: "Subtasks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Subjects",
                table: "Subjects",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentSubtasks",
                table: "StudentSubtasks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentExams",
                table: "StudentExams",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Students",
                table: "Students",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Exams",
                table: "Exams",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Courses",
                table: "Courses",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Classes",
                table: "Classes",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_StudentExams_StudentId",
                table: "StudentExams",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassCourse_Classes_ClassesId",
                table: "ClassCourse",
                column: "ClassesId",
                principalTable: "Classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassCourse_Courses_CoursesId",
                table: "ClassCourse",
                column: "CoursesId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_Teachers_TeacherId",
                table: "Classes",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Subjects_SubjectId",
                table: "Courses",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseTeacher_Courses_CoursesId",
                table: "CourseTeacher",
                column: "CoursesId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseTeacher_Teachers_TeachersId",
                table: "CourseTeacher",
                column: "TeachersId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Exams_Courses_CourseId",
                table: "Exams",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Exams_Teachers_TeacherId",
                table: "Exams",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentClass_Classes_ClassesId",
                table: "StudentClass",
                column: "ClassesId",
                principalTable: "Classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentClass_Students_StudentsId",
                table: "StudentClass",
                column: "StudentsId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentExams_Exams_ExamId",
                table: "StudentExams",
                column: "ExamId",
                principalTable: "Exams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentExams_Students_StudentId",
                table: "StudentExams",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentSubtasks_StudentExams_StudentExamId",
                table: "StudentSubtasks",
                column: "StudentExamId",
                principalTable: "StudentExams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentSubtasks_Subtasks_SubtaskId",
                table: "StudentSubtasks",
                column: "SubtaskId",
                principalTable: "Subtasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Subtasks_Exams_ExamId",
                table: "Subtasks",
                column: "ExamId",
                principalTable: "Exams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
