import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { FormsModule } from '@angular/forms';
import { AdminService, UsersPagedResultDto } from '../../services/admin.service';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    FormsModule,
  ],
  templateUrl: './admin-users.component.html',
  styleUrls: ['./admin-users.component.css'],
})
export class AdminUsersComponent {
  private admin = inject(AdminService);
  displayedColumns = ['userName', 'email', 'createdAt', 'role', 'actions'];
  items = signal<UsersPagedResultDto['items']>([]);
  total = signal(0);
  page = 1;
  pageSize = 10;
  name = '';
  email = '';
  role = '';

  ngOnInit() {
    this.load();
  }

  load() {
    this.admin
      .getUsers({ page: this.page, pageSize: this.pageSize, name: this.name, email: this.email, role: this.role })
      .subscribe((res) => {
        this.items.set(res.items);
        this.total.set(res.total);
      });
  }

  onPage(ev: PageEvent) {
    this.page = ev.pageIndex + 1;
    this.pageSize = ev.pageSize;
    this.load();
  }

  onRoleChange(row: { id: number; role: string }) {
    this.admin.changeRole({ userId: row.id, role: row.role }).subscribe(() => this.load());
  }

  remove(row: { id: number }) {
    this.admin.deleteUser(row.id).subscribe(() => this.load());
  }
}
