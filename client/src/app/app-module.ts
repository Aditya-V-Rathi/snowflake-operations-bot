import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { App } from './app';
import { DashboardComponent } from './components/dashboard/dashboard';
import { provideHttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

@NgModule({
  declarations: [
    App,
    DashboardComponent
  ],
  imports: [
    BrowserModule,
    FormsModule
  ],
  providers: [
    provideHttpClient()
  ],
  bootstrap: [App]
})
export class AppModule {}