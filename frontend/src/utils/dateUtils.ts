export const formatDate = (date: Date | string | null | undefined): string => {
  if (!date) return '';
  const d = date instanceof Date ? date : new Date(date);
  return d.toLocaleDateString('en-GB'); // dd/MM/yyyy
};

export const parseDate = (dateString: string): Date | null => {
  if (!dateString) return null;
  const [day, month, year] = dateString.split('/');
  const d = new Date(Number(year), Number(month) - 1, Number(day));
  // Check if the date is valid
  if (isNaN(d.getTime())) {
    return null;
  }
  return d;
};
