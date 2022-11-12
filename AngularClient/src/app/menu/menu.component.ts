import { Component, OnInit } from '@angular/core';
import { AuthenticationService } from '../shared/services/authentication.service';

@Component({
  selector: 'app-menu',
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.css']
})
export class MenuComponent implements OnInit {
  isCollapsed: boolean = false;
  public isUserAuthenticated: boolean;

  constructor(private authService: AuthenticationService) { }

  ngOnInit(): void {
    this.authService.authChanged.subscribe(res => {
      this.isUserAuthenticated = res;
    })
  }

}
