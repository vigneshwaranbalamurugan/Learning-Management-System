import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormInput } from './form-input';
import { vi } from 'vitest';

describe('FormInput', () => {
  let component: FormInput;
  let fixture: ComponentFixture<FormInput>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FormInput]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(FormInput);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should toggle password visibility', () => {
    component.type = 'password';
    expect(component['showPassword']).toBe(false);
    expect(component['inputType']).toBe('password');
    
    component['togglePasswordVisibility']();
    expect(component['showPassword']).toBe(true);
    expect(component['inputType']).toBe('text');
  });

  it('should emit value on input change', () => {
    vi.spyOn(component.valueChange, 'emit');
    
    const inputElement = document.createElement('input');
    inputElement.value = 'test value';
    
    const event = { target: inputElement } as any;
    component['onInput'](event);
    
    expect(component.value).toBe('test value');
    expect(component.valueChange.emit).toHaveBeenCalledWith('test value');
  });
});
