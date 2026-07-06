import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ConfirmModal } from './confirm-modal';
import { ComponentRef } from '@angular/core';
import { vi } from 'vitest';

describe('ConfirmModal', () => {
  let component: ConfirmModal;
  let fixture: ComponentFixture<ConfirmModal>;
  let componentRef: ComponentRef<ConfirmModal>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ConfirmModal]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(ConfirmModal);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should not render anything if isOpen is false', () => {
    componentRef.setInput('isOpen', false);
    fixture.detectChanges();
    const modalContent = fixture.nativeElement.querySelector('.fixed.inset-0');
    expect(modalContent).toBeNull();
  });

  it('should render modal when isOpen is true', () => {
    componentRef.setInput('isOpen', true);
    fixture.detectChanges();
    const modalContent = fixture.nativeElement.querySelector('.fixed.inset-0');
    expect(modalContent).toBeTruthy();
  });

  it('should emit confirm event when confirm button is clicked', () => {
    componentRef.setInput('isOpen', true);
    fixture.detectChanges();
    
    vi.spyOn(component.confirm, 'emit');
    const buttons = fixture.nativeElement.querySelectorAll('button');
    const confirmButton = buttons[1];
    
    confirmButton.click();
    expect(component.confirm.emit).toHaveBeenCalled();
  });

  it('should emit cancel event when cancel button is clicked', () => {
    componentRef.setInput('isOpen', true);
    fixture.detectChanges();
    
    vi.spyOn(component.cancel, 'emit');
    const buttons = fixture.nativeElement.querySelectorAll('button');
    const cancelButton = buttons[0];
    
    cancelButton.click();
    expect(component.cancel.emit).toHaveBeenCalled();
  });
});
