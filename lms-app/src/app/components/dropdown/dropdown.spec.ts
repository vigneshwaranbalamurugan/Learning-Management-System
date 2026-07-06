import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Dropdown } from './dropdown';
import { ComponentRef } from '@angular/core';
import { vi } from 'vitest';

describe('Dropdown', () => {
  let component: Dropdown;
  let fixture: ComponentFixture<Dropdown>;
  let componentRef: ComponentRef<Dropdown>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Dropdown]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(Dropdown);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;
    
    componentRef.setInput('options', [
      { value: '1', label: 'Option 1' },
      { value: '2', label: 'Option 2' }
    ]);
    
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should toggle dropdown', () => {
    expect(component['isOpen']).toBe(false);
    component['toggleDropdown']();
    expect(component['isOpen']).toBe(true);
    component['toggleDropdown']();
    expect(component['isOpen']).toBe(false);
  });

  it('should select option, emit value, and close dropdown', () => {
    vi.spyOn(component.valueChange, 'emit');
    component['isOpen'] = true;
    
    component['selectOption']('1');
    
    expect(component.value).toBe('1');
    expect(component.valueChange.emit).toHaveBeenCalledWith('1');
    expect(component['isOpen']).toBe(false);
  });

  it('should close on outside click', () => {
    component['isOpen'] = true;
    const outsideElement = document.createElement('div');
    const event = new MouseEvent('click');
    Object.defineProperty(event, 'target', { value: outsideElement, enumerable: true });
    
    component['onClickOutside'](event);
    expect(component['isOpen']).toBe(false);
  });
});
