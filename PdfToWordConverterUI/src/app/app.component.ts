import { Component } from '@angular/core';
import { PdfConverterComponent } from './pdf-converter/pdf-converter.component';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: true,
  imports: [PdfConverterComponent]  // Import the standalone component
})
export class AppComponent {
  title = 'PdfToWordConverterUI';
}
