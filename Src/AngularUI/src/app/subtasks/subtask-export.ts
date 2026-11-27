import { Subtask } from '../models/subtask.model';

/** A table cell must stay on one line and must not contain unescaped pipes. */
function toCell(text: string): string {
  return text.trim().replace(/\|/g, '\|').replace(/\r\n|\r|\n/g, '<br>');
}

/** Builds the Markdown document with the grading criteria of an exam. */
export function subtasksToMarkdown(examDescription: string, subtasks: Subtask[]): string {
  const totalPoints = subtasks.reduce((sum, s) => sum + (s.bonus ? 0 : s.points), 0);
  const bonusPoints = subtasks.reduce((sum, s) => sum + (s.bonus ? s.points : 0), 0);

  const lines = ['# Bewertungskriterien', ''];
  if (examDescription) {
    lines.push(examDescription, '');
  }

  lines.push(
    '| Nr. | Task | Punkte | Erwartungshorizont |',
    '| --- | ---- | ------ | ------------------ |'
  );
  for (const s of subtasks) {
    const points = `${s.points}${s.bonus ? ' (Bonus)' : ''}`;
    lines.push(`| ${s.seqNo} | ${toCell(s.description)} | ${points} | ${toCell(s.expectedResult ?? '')} |`);
  }

  lines.push('', `Gesamtpunkte (ohne Bonus): ${totalPoints}`);
  if (bonusPoints > 0) {
    lines.push('', `Bonuspunkte: ${bonusPoints}`);
  }

  return lines.join('\n') + '\n';
}
