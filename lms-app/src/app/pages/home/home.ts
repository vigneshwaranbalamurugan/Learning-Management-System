import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Navbar } from '../../components/navbar/navbar';
import { HeroSection } from '../../components/hero-section/hero-section';
import { FeaturesSection } from '../../components/features-section/features-section';
import { AboutSection } from '../../components/about-section/about-section';
import { FeedbackSection } from '../../components/feedback-section/feedback-section';
import { Footer } from '../../components/footer/footer';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    CommonModule,
    Navbar,
    HeroSection,
    FeaturesSection,
    AboutSection,
    FeedbackSection,
    Footer,
  ],
  templateUrl: './home.html',
})
export class Home {}
