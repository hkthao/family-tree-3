function formatDate(date: string | Date): string {
  const d = typeof date === 'string' ? new Date(date) : date;
  const year = d.getFullYear();
  const month = (d.getMonth() + 1).toString().padStart(2, '0'); // month 0-based
  const day = d.getDate().toString().padStart(2, '0');
  return `${year}-${month}-${day}`;
}

function addDays(date: string | Date, days: number): string {
  const d = typeof date === 'string' ? new Date(date) : new Date(date.getTime());
  d.setDate(d.getDate() + days);
  return formatDate(d);
}

export {
  formatDate,
  addDays
}