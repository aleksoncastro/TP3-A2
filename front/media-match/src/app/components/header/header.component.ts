import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { BrnNavigationMenuImports } from '@spartan-ng/brain/navigation-menu';
import { BrnButtonImports } from '@spartan-ng/brain/button';
import { BrnLabelImports } from '@spartan-ng/brain/label';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, BrnNavigationMenuImports, BrnButtonImports, BrnLabelImports],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})
export class HeaderComponent {
  private readonly router = inject(Router);
  q = '';

  onSearch() {
    const queryParams = this.q ? { q: this.q, page: 1 } : {};
    this.router.navigate(['/movie'], { queryParams });
  }
}
