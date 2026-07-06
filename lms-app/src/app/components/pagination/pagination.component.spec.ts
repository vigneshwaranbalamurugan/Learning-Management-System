import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PaginationComponent } from './pagination.component';
import { ComponentRef } from '@angular/core';
import { vi } from 'vitest';

describe('PaginationComponent', () => {
  let component: PaginationComponent;
  let fixture: ComponentFixture<PaginationComponent>;
  let componentRef: ComponentRef<PaginationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PaginationComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(PaginationComponent);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;
    
    componentRef.setInput('pageNumber', 1);
    componentRef.setInput('totalPages', 5);
    componentRef.setInput('totalCount', 50);
    componentRef.setInput('pageSize', 10);
    
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should not emit changePage if page is out of bounds', () => {
    vi.spyOn(component.pageChange, 'emit');
    
    component.changePage(0); // Less than 1
    expect(component.pageChange.emit).not.toHaveBeenCalled();
    
    component.changePage(6); // Greater than totalPages (5)
    expect(component.pageChange.emit).not.toHaveBeenCalled();
  });

  it('should not emit changePage if page is same as current', () => {
    vi.spyOn(component.pageChange, 'emit');
    
    component.changePage(1); // Same as current page (1)
    expect(component.pageChange.emit).not.toHaveBeenCalled();
  });

  it('should emit changePage for valid page', () => {
    vi.spyOn(component.pageChange, 'emit');
    
    component.changePage(2);
    expect(component.pageChange.emit).toHaveBeenCalledWith(2);
  });
});
