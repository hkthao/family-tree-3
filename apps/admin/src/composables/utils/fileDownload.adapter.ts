export interface IFileDownloadAdapter {
  downloadJson(data: any, fileName: string): void;
}

export const defaultFileDownloadAdapter: IFileDownloadAdapter = {
  downloadJson: (data: any, fileName: string) => {
    const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.setAttribute('download', fileName);
    document.body.appendChild(link);
    link.click();
    link.remove();
    window.URL.revokeObjectURL(url);
  },
};