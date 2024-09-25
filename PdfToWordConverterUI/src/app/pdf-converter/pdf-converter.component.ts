import { Component } from '@angular/core';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-pdf-converter',
  standalone: true,
  imports: [HttpClientModule],
  templateUrl: './pdf-converter.component.html',
  styleUrls: ['./pdf-converter.component.css'],
})
export class PdfConverterComponent {
  selectedFile: File | null = null;
  resultMessage: string = '';
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  onFileSelected(event: any) {
    this.selectedFile = event.target.files[0];
  }

  onUpload() {
    if (!this.selectedFile) {
      this.resultMessage = 'Please select a file first!';
      return;
    }

    const formData = new FormData();
    formData.append('pdfFile', this.selectedFile);

    this.http.post(`${this.apiUrl}/PdfToWord/convert`, formData, { responseType: 'blob' })
      .subscribe({
        next: (response) => {
          if (!response || response.size === 0) {
            this.resultMessage = 'File conversion failed. Empty or invalid response.';
            return;
          }
          const url = window.URL.createObjectURL(response);
          const anchor = document.createElement('a');
          anchor.href = url;
          anchor.download = 'converted.docx';
          anchor.click();
          this.resultMessage = 'Conversion Successful!';
        },
        error: () => {
          this.resultMessage = 'Conversion Failed!';
        }
      });
  }

  resetFile() {
    this.selectedFile = null;
    this.resultMessage = '';
  }
}
